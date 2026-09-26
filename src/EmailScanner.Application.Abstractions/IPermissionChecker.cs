namespace EmailScanner.Application.Abstractions;

public interface IPermissionChecker { bool HasPermission(string permission); }
