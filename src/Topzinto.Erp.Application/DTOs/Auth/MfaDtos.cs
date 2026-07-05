namespace Topzinto.Erp.Application.DTOs.Auth;

public record MfaStatusDto(bool Enabled);

public record MfaSetupDto(string SharedKey, string AuthenticatorUri);

public record MfaEnableRequest(string Code);

public record MfaDisableRequest(string Code);

public record MfaVerifyRequest(string MfaToken, string Code, bool RememberMe = false);

public record MfaChallengeResponse(string MfaToken, string Message);
