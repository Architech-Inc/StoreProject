# ADR-004: Anti-Virus File Inspection with ClamAV Sidecar & Dev Fallback

## Status
**Accepted** (2026-01-20)

## Context
The platform allows operators to upload product catalog photos, vendor invoice PDFs, customer profile avatars, and payment receipts. Public-facing and authenticated upload endpoints represent an attack surface for malware persistence, web shells, and malicious SVG/PDF scripts.

We needed a robust anti-virus scanning strategy that protects multi-tenant file storage without blocking local developer workflows where running a full ClamAV scanning daemon is unnecessary and memory-intensive.

## Decision
We adopted **ClamAV REST sidecar scanning with an environment-enforced provider abstraction**:

1. **Provider Abstraction (`IVirusScanner`)**:
   - `ClamAvVirusScanner`: Streams file bytes over HTTP to an isolated `clamav-rest` sidecar container. Returns scan status (clean, infected, or unreachable).
   - `NoOpVirusScanner`: Bypass scanner used strictly for local offline developer environments.

2. **Strict Production Environment Guards**:
   - On application startup (`Program.cs`), if `ASPNETCORE_ENVIRONMENT != "Development"` and `Antivirus:Provider == "NoOp"`, the application throws a fatal exception and refuses to boot.
   - Production container stacks are required to configure `Antivirus:ClamAV:BaseUrl` pointing to the ClamAV sidecar.

3. **Multi-Stage Upload Validation Pipeline**:
   - File uploads in `FilesController` pass through four sequential security checks:
     1. **Extension Allowlisting**: Only vetted image/document extensions (`.jpg`, `.jpeg`, `.png`, `.webp`, `.pdf`).
     2. **Magic Number Header Validation**: Inspects byte headers to prevent extension spoofing (e.g. executable disguised as `.png`).
     3. **Path Traversal Sanitization**: Eliminates `../`, null bytes, and path separators; generates cryptographic UUID filenames.
     4. **Antivirus Stream Inspection**: Calls `IVirusScanner.ScanAsync()` before the file stream is persisted to disk or cloud storage.

## Alternatives Considered
- **Native Host ClamAV CLI (`clamscan`)**: Spawning a shell command per upload. Rejected due to severe process-forking overhead and cross-platform inconsistencies between Linux containers and Windows developer workstations.
- **Cloud-Only Scanning (e.g. AWS GuardDuty / VirusTotal API)**: Rejected due to recurring SaaS costs, data residency concerns for regional documents, and external API latency.

## Consequences
### Positive
- **Strong Malware Defense**: Malicious files are rejected at the HTTP upload boundary before they can be stored or served to users.
- **Zero Friction for Developers**: Developers run with `Antivirus:Provider=NoOp` without needing a local 1GB ClamAV container.
- **Fail-Secure Architecture**: Production deployments cannot accidentally launch with virus scanning disabled.

### Trade-offs
- ClamAV container requires ~800MB–1.2GB RAM for virus definition signatures in production.
- Slight latency overhead (~50–150ms) on file upload requests while bytes stream through the scanner.
