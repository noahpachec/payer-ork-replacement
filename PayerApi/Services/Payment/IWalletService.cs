using Ork.Entities.Payer;

namespace Ork.Services.Payment;

public interface IWalletService
{
    void CreateWallet(string index, string id, byte[] vProof, byte[] yProof, string? customerId = null);
    void UpdateWalletId(string index, string newId, byte[] vProof, byte[] yProof, string? customerId = null);
    Wallet? GetWallet(string index);
    Wallet? GetWalletById(string id);

    // Exemplar addition (not in ork)
    List<Wallet> GetAllWallets();
}
