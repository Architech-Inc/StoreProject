namespace Store.Models.Security;

/// <summary>
/// SEC-23 — verdict returned by <see cref="Interfaces.Services.IDeviceBindingGuard"/>
/// when checking whether a request comes from an enrolled device.
///
/// The states are deliberately ordered from most-permissive to least:
/// the guard returns the worst-case verdict it finds so the caller can
/// branch on the single value.
/// </summary>
public enum DeviceBindingResult
{
    /// <summary>
    /// Device is enrolled AND has previously authenticated with WebAuthn.
    /// Refresh / biometric-login may proceed without step-up.
    /// </summary>
    EnrolledWithWebAuthn,

    /// <summary>
    /// Device is enrolled (we've seen this fingerprint before for this user)
    /// but the user has never completed a WebAuthn assertion from this
    /// device — only password logins. Refresh may proceed; biometric
    /// assertion is permitted but will stamp <c>LastWebAuthnAtUtc</c> on
    /// success.
    /// </summary>
    PasswordOnly,

    /// <summary>
    /// The fingerprint is unknown for this user. This is the SEC-23
    /// critical case: an attacker is replaying a stolen refresh token
    /// from a device the user has never used. Refresh must be denied so
    /// the user is forced to re-authenticate (and re-enroll) from the
    /// real device.
    /// </summary>
    UnknownDevice,

    /// <summary>
    /// The device is enrolled but has been soft-revoked by the user.
    /// Same effect as UnknownDevice: deny refresh + require re-auth.
    /// </summary>
    RevokedDevice,
}
