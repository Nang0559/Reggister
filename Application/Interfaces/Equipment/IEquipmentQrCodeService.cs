namespace FVN_REGISTER.Application.Interfaces.Equipment;

public interface IEquipmentQrCodeService
{
    byte[] CreatePng(string payload);
}
