using QRCoder;

namespace PiratenKarte.Client.Models;

public static class QrCodeGenerator {
    public static byte[] Generate(Guid id, string domain, int pixelsPerModule = 10) {
        var qrGen = new QRCodeGenerator();
        var payload = new PayloadGenerator.Url($"https://{domain}/mapobjects/view/{id}");
        var data = qrGen.CreateQrCode(payload);
        var qrCode = new PngByteQRCode(data);
        return qrCode.GetGraphic(pixelsPerModule, false);
    }
}