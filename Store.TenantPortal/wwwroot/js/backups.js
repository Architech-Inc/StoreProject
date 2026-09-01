// Backups UI Interaction

document.addEventListener('DOMContentLoaded', () => {
    initTriggerBackup();
    initS3Configuration();
    initScheduleSave();
    initDisconnectProvider();
});

function initTriggerBackup() {
    const btn = document.getElementById('btnTriggerBackup');
    if (!btn) return;

    btn.addEventListener('click', async () => {
        ClexAn.confirm(
            'Are you sure you want to trigger a manual backup now?',
            'Trigger Backup',
            async () => {
                const originalText = btn.innerHTML;
                btn.disabled = true;
                btn.innerHTML = `<svg class="animate-spin" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="display:inline-block; vertical-align:middle; margin-right:8px;"><circle cx="12" cy="12" r="10" stroke-opacity="0.25"></circle><path d="M12 2a10 10 0 0 1 10 10" stroke-linecap="round"></path></svg> Generating Snapshot...`;

                try {
                    const response = await fetch('/api/backups/trigger', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' }
                    });

                    const result = await response.json();
                    if (result.success) {
                        ClexAn.alert(result.message, 'Backup Complete', 'success');
                        setTimeout(() => location.reload(), 2000);
                    } else {
                        ClexAn.alert(result.message || 'Backup failed.', 'Error', 'error');
                        btn.disabled = false;
                        btn.innerHTML = originalText;
                    }
                } catch (e) {
                    ClexAn.alert('An unexpected error occurred.', 'Error', 'error');
                    btn.disabled = false;
                    btn.innerHTML = originalText;
                }
            }
        );
    });
}

function initS3Configuration() {
    const btnAddS3 = document.getElementById('btnAddS3');
    if (!btnAddS3) return;

    btnAddS3.addEventListener('click', () => {
        // We'll use the ClexAn dialog system for a custom form
        const formHtml = `
            <div style="text-align: left; margin-bottom: 16px;">
                <label style="display:block; margin-bottom:4px; font-size:13px; color:var(--p-text-muted);">Endpoint URL</label>
                <input type="text" id="s3Endpoint" class="portal-input" value="https://s3.amazonaws.com" style="width:100%;" />
            </div>
            <div style="text-align: left; margin-bottom: 16px;">
                <label style="display:block; margin-bottom:4px; font-size:13px; color:var(--p-text-muted);">Region</label>
                <input type="text" id="s3Region" class="portal-input" value="us-east-1" style="width:100%;" />
            </div>
            <div style="text-align: left; margin-bottom: 16px;">
                <label style="display:block; margin-bottom:4px; font-size:13px; color:var(--p-text-muted);">Bucket Name</label>
                <input type="text" id="s3Bucket" class="portal-input" placeholder="my-backups-bucket" style="width:100%;" />
            </div>
            <div style="text-align: left; margin-bottom: 16px;">
                <label style="display:block; margin-bottom:4px; font-size:13px; color:var(--p-text-muted);">Access Key ID</label>
                <input type="text" id="s3AccessKey" class="portal-input" placeholder="AKIA..." style="width:100%;" />
            </div>
            <div style="text-align: left; margin-bottom: 16px;">
                <label style="display:block; margin-bottom:4px; font-size:13px; color:var(--p-text-muted);">Secret Access Key</label>
                <input type="password" id="s3SecretKey" class="portal-input" style="width:100%;" />
            </div>
        `;

        const modal = document.createElement('div');
        modal.className = 'clexan-modal-overlay';
        modal.innerHTML = `
            <div class="clexan-modal-card">
                <div class="clexan-modal-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#4ade80" stroke-width="2"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path><polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline><line x1="12" y1="22.08" x2="12" y2="12"></line></svg>
                </div>
                <h3 class="clexan-modal-title">Configure S3 / MinIO Provider</h3>
                <p class="clexan-modal-message">Enter your S3 compatible credentials. Secrets will be encrypted at rest.</p>
                <div class="clexan-modal-custom-content">
                    ${formHtml}
                </div>
                <div class="clexan-modal-actions">
                    <button class="btn-glass" id="btnCancelS3">Cancel</button>
                    <button class="btn-primary-glow" id="btnSaveS3">Save Configuration</button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        setTimeout(() => modal.classList.add('visible'), 10);

        const close = () => {
            modal.classList.remove('visible');
            setTimeout(() => modal.remove(), 300);
        };

        document.getElementById('btnCancelS3').addEventListener('click', close);
        document.getElementById('btnSaveS3').addEventListener('click', async () => {
            const req = {
                endpointUrl: document.getElementById('s3Endpoint').value.trim(),
                region: document.getElementById('s3Region').value.trim(),
                bucketName: document.getElementById('s3Bucket').value.trim(),
                accessKeyId: document.getElementById('s3AccessKey').value.trim(),
                secretAccessKey: document.getElementById('s3SecretKey').value
            };

            if (!req.bucketName || !req.accessKeyId || !req.secretAccessKey) {
                ClexAn.alert('Please fill out all required fields.', 'Validation Error', 'warning');
                return;
            }

            document.getElementById('btnSaveS3').innerHTML = 'Saving...';
            document.getElementById('btnSaveS3').disabled = true;

            try {
                const response = await fetch('/api/backups/providers/s3', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(req)
                });
                const result = await response.json();
                
                close();

                if (result.success) {
                    ClexAn.alert(result.message, 'Success', 'success');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    ClexAn.alert(result.message || 'Failed to save S3 configuration.', 'Error', 'error');
                }
            } catch (e) {
                close();
                ClexAn.alert('An unexpected error occurred.', 'Error', 'error');
            }
        });
    });
}

function initScheduleSave() {
    const btn = document.getElementById('btnSaveSchedule');
    if (!btn) return;

    btn.addEventListener('click', async () => {
        const req = {
            isEnabled: document.getElementById('scheduleEnabled').checked,
            frequency: document.getElementById('scheduleFrequency').value,
            retentionCount: parseInt(document.getElementById('scheduleRetention').value, 10) || 14
        };

        const originalText = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = 'Saving...';

        try {
            const response = await fetch('/api/backups/schedule', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(req)
            });

            const result = await response.json();
            btn.innerHTML = originalText;
            btn.disabled = false;

            if (result.success) {
                ClexAn.alert(result.message, 'Schedule Saved', 'success');
            } else {
                ClexAn.alert(result.message || 'Failed to update schedule.', 'Error', 'error');
            }
        } catch (e) {
            ClexAn.alert('An unexpected error occurred.', 'Error', 'error');
            btn.innerHTML = originalText;
            btn.disabled = false;
        }
    });
}

function initDisconnectProvider() {
    document.querySelectorAll('.disconnect-provider').forEach(btn => {
        btn.addEventListener('click', () => {
            const type = btn.getAttribute('data-type');
            ClexAn.confirm(
                `Are you sure you want to disconnect ${type}? This will stop sending backups to this destination.`,
                'Disconnect Provider',
                async () => {
                    btn.disabled = true;
                    btn.innerHTML = '...';

                    try {
                        const response = await fetch(`/api/backups/providers/${type}`, {
                            method: 'DELETE'
                        });

                        const result = await response.json();
                        if (result.success) {
                            ClexAn.alert(result.message, 'Disconnected', 'success');
                            setTimeout(() => location.reload(), 1500);
                        } else {
                            ClexAn.alert(result.message || 'Failed to disconnect.', 'Error', 'error');
                            btn.disabled = false;
                            btn.innerHTML = 'Disconnect';
                        }
                    } catch (e) {
                        ClexAn.alert('An unexpected error occurred.', 'Error', 'error');
                        btn.disabled = false;
                        btn.innerHTML = 'Disconnect';
                    }
                }
            );
        });
    });
}
