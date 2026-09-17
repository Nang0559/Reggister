using FVN_REGISTER.Application.Interfaces.Equipment;
using QRCoder;

namespace FVN_REGISTER.Infrastructure.Services.Equipment;

public sealed class QrCodeService : IEquipmentQrCodeService
{
    public byte[] CreatePng(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("QR payload is required.");
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(8);
    }
}
