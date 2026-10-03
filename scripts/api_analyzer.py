#!/usr/bin/env python3
"""
API Endpoint and Client Call Analyzer

Analyzes C# ASP.NET Core API controllers and Blazor/Razor UI service client calls,
matching them up and producing a markdown report.

Usage:
    python scripts/api_analyzer.py [--api-dir PATH] [--ui-dir PATH] [--output PATH]
"""

import os
import re
import sys
import argparse

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, ".."))

DEFAULT_API_DIR = os.path.join(REPO_ROOT, "Store.API", "Controllers")
DEFAULT_UI_DIR = os.path.join(REPO_ROOT, "Store.UI", "Services")
DEFAULT_OUTPUT_FILE = os.path.join(REPO_ROOT, "api_analysis_report.md")


def extract_endpoints(api_dir):
    endpoints = []
    if not os.path.exists(api_dir):
        print(f"Warning: API directory '{api_dir}' does not exist.", file=sys.stderr)
        return endpoints

    for root, _, files in os.walk(api_dir):
        for file in files:
            if not file.endswith(".cs"):
                continue
            path = os.path.join(root, file)
            with open(path, "r", encoding="utf-8") as f:
                content = f.read()

                # Find base route
                route_match = re.search(r'\[Route\("([^"]+)"\)\]', content)
                base_route = route_match.group(1) if route_match else "api/[controller]"
                controller_name = file.replace(".cs", "").replace("Controller", "")
                base_route = base_route.replace("[controller]", controller_name.lower())

                # Find all HTTP action methods
                methods = re.finditer(
                    r'\[Http(Get|Post|Put|Delete|Patch)(?:\("([^"]+)"\))?\][\s\S]*?public\s+(?:async\s+)?(?:Task<)?([^\s>]+)[^>]*?>?\s+(\w+)\s*\(([^)]*)\)',
                    content,
                )
                for m in methods:
                    http_method = m.group(1).upper()
                    sub_route = m.group(2) if m.group(2) else ""
                    return_type = m.group(3)
                    method_name = m.group(4)
                    params = m.group(5)

                    full_route = base_route
                    if sub_route:
                        full_route += "/" + sub_route

                    endpoints.append(
                        {
                            "controller": controller_name,
                            "method": http_method,
                            "route": full_route,
                            "return_type": return_type,
                            "method_name": method_name,
                            "params": params,
                            "file": file,
                        }
                    )
    return endpoints


def extract_client_calls(ui_dir):
    calls = []
    if not os.path.exists(ui_dir):
        print(f"Warning: UI directory '{ui_dir}' does not exist.", file=sys.stderr)
        return calls

    for root, _, files in os.walk(ui_dir):
        for file in files:
            if not file.endswith(".cs"):
                continue
            path = os.path.join(root, file)
            with open(path, "r", encoding="utf-8") as f:
                content = f.read()

                # Find HttpClient calls
                matches = re.finditer(
                    r'(Get|Post|Put|Delete)(?:AsJson)?Async(?:<([^>]+)>)?\(\s*[\$]?["\']([^"\']+)["\']',
                    content,
                )
                for m in matches:
                    method = m.group(1).upper()
                    generic_type = m.group(2)
                    url = m.group(3)

                    calls.append(
                        {
                            "method": method,
                            "url": url,
                            "generic_type": generic_type,
                            "file": file,
                        }
                    )
    return calls


def generate_report(api_dir, ui_dir, output_file):
    endpoints = extract_endpoints(api_dir)
    client_calls = extract_client_calls(ui_dir)

    os.makedirs(os.path.dirname(os.path.abspath(output_file)), exist_ok=True)
    with open(output_file, "w", encoding="utf-8") as f:
        f.write("# API Endpoint and Client Call Analysis\n\n")
        f.write(
            "This report analyzes what happens when an endpoint is hit, its inputs/outputs, and how the client processes it.\n\n"
        )

        # Group by controller
        controllers = {}
        for ep in endpoints:
            c = ep["controller"]
            if c not in controllers:
                controllers[c] = []
            controllers[c].append(ep)

        for c, eps in sorted(controllers.items()):
            f.write(f"## {c} Controller\n\n")
            for ep in eps:
                f.write(f"### `{ep['method']} {ep['route']}`\n")
                f.write(f"- **Backend Method**: `{ep['method_name']}` in `{ep['file']}`\n")
                f.write(
                    f"- **Receives (Parameters)**: `{ep['params'] if ep['params'].strip() else 'None'}`\n"
                )
                f.write(f"- **Returns**: `{ep['return_type']}`\n")

                # Find matching client calls
                matching_calls = []
                for call in client_calls:
                    if call["method"] == ep["method"]:
                        # Basic route prefix match
                        clean_route = ep["route"].split("{")[0].lower()
                        if call["url"].lower().startswith(clean_route):
                            matching_calls.append(call)

                if matching_calls:
                    f.write("- **Client Call(s)**:\n")
                    for mc in matching_calls:
                        f.write(f"  - Found in `{mc['file']}` using `{mc['method']}`\n")
                        f.write(
                            f"  - Client expects generic type: `{mc['generic_type'] if mc['generic_type'] else 'Unknown'}`\n"
                        )
                else:
                    f.write(
                        "- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).\n"
                    )

                f.write("\n")

    print(f"Report generated successfully: {output_file} ({len(endpoints)} endpoints mapped).")


def main():
    parser = argparse.ArgumentParser(
        description="Analyze C# API endpoints and UI service client calls."
    )
    parser.add_argument(
        "--api-dir",
        default=DEFAULT_API_DIR,
        help=f"Path to Store.API Controllers directory (default: {DEFAULT_API_DIR})",
    )
    parser.add_argument(
        "--ui-dir",
        default=DEFAULT_UI_DIR,
        help=f"Path to Store.UI Services directory (default: {DEFAULT_UI_DIR})",
    )
    parser.add_argument(
        "--output",
        default=DEFAULT_OUTPUT_FILE,
        help=f"Output markdown report path (default: {DEFAULT_OUTPUT_FILE})",
    )
    args = parser.parse_args()

    generate_report(args.api_dir, args.ui_dir, args.output)


if __name__ == "__main__":
    main()
