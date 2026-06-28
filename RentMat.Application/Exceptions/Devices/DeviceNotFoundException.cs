namespace RentMat.Application.Exceptions.Devices;

public class DeviceNotFoundException (int id) : Exception($"Device with id {id} was not found"), IExceptionBase
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public string Title => "Device was not found";
}