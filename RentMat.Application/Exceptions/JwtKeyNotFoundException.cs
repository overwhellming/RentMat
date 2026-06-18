namespace RentMat.Application.Exceptions;

public class JwtKeyNotFoundException() : Exception("JWT key was not found in configuration");