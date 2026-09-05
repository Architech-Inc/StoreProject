import os
import re

API_DIR = r"c:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API\Controllers"
UI_DIR = r"c:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.UI\Services"
OUTPUT_FILE = r"c:\Users\Rodern\source\repos\Architech-Inc\StoreProject\api_analysis_report.md"

def extract_endpoints():
    endpoints = []
    for root, _, files in os.walk(API_DIR):
        for file in files:
            if not file.endswith('.cs'): continue
            path = os.path.join(root, file)
            with open(path, 'r', encoding='utf-8') as f:
                content = f.read()
                
                # find route
                route_match = re.search(r'\[Route\("([^"]+)"\)\]', content)
                base_route = route_match.group(1) if route_match else "api/[controller]"
                controller_name = file.replace('.cs', '').replace('Controller', '')
                base_route = base_route.replace("[controller]", controller_name.lower())
                
                # find all methods
                methods = re.finditer(r'\[Http(Get|Post|Put|Delete|Patch)(?:\("([^"]+)"\))?\][\s\S]*?public\s+(?:async\s+)?(?:Task<)?([^\s>]+)[^>]*?>?\s+(\w+)\s*\(([^)]*)\)', content)
                for m in methods:
                    http_method = m.group(1).upper()
                    sub_route = m.group(2) if m.group(2) else ""
                    return_type = m.group(3)
                    method_name = m.group(4)
                    params = m.group(5)
                    
                    full_route = base_route
                    if sub_route:
                        full_route += "/" + sub_route
                    
                    endpoints.append({
                        "controller": controller_name,
                        "method": http_method,
                        "route": full_route,
                        "return_type": return_type,
                        "method_name": method_name,
                        "params": params,
                        "file": file
                    })
    return endpoints

def extract_client_calls():
    calls = []
    for root, _, files in os.walk(UI_DIR):
        for file in files:
            if not file.endswith('.cs'): continue
            path = os.path.join(root, file)
            with open(path, 'r', encoding='utf-8') as f:
                content = f.read()
                
                # find httpClient calls
                matches = re.finditer(r'(Get|Post|Put|Delete)(?:AsJson)?Async(?:<([^>]+)>)?\(\s*[\$]?["\']([^"\']+)["\']', content)
                for m in matches:
                    method = m.group(1).upper()
                    generic_type = m.group(2)
                    url = m.group(3)
                    
                    calls.append({
                        "method": method,
                        "url": url,
                        "generic_type": generic_type,
                        "file": file
                    })
    return calls

def generate_report():
    endpoints = extract_endpoints()
    client_calls = extract_client_calls()
    
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        f.write("# API Endpoint and Client Call Analysis\n\n")
        f.write("This report analyzes what happens when an endpoint is hit, its inputs/outputs, and how the client processes it.\n\n")
        
        # Group by controller
        controllers = {}
        for ep in endpoints:
            c = ep["controller"]
            if c not in controllers: controllers[c] = []
            controllers[c].append(ep)
            
        for c, eps in controllers.items():
            f.write(f"## {c} Controller\n\n")
            for ep in eps:
                f.write(f"### `{ep['method']} {ep['route']}`\n")
                f.write(f"- **Backend Method**: `{ep['method_name']}` in `{ep['file']}`\n")
                f.write(f"- **Receives (Parameters)**: `{ep['params'] if ep['params'].strip() else 'None'}`\n")
                f.write(f"- **Returns**: `{ep['return_type']}`\n")
                
                # Find matching client call roughly
                matching_calls = []
                for call in client_calls:
                    if call['method'] == ep['method']:
                        # very basic match
                        if call['url'].lower().startswith(ep['route'].split('{')[0].lower()):
                            matching_calls.append(call)
                            
                if matching_calls:
                    f.write("- **Client Call(s)**:\n")
                    for mc in matching_calls:
                        f.write(f"  - Found in `{mc['file']}` using `{mc['method']}`\n")
                        f.write(f"  - Client expects generic type: `{mc['generic_type'] if mc['generic_type'] else 'Unknown'}`\n")
                else:
                    f.write("- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).\n")
                
                f.write("\n")

if __name__ == "__main__":
    generate_report()
    print("Report generated successfully.")
