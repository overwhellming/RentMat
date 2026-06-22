namespace RentMat.Application.Exceptions.Authentication;

public class JwtKeyNotFoundException() : Exception("JWT key was not found in configuration");