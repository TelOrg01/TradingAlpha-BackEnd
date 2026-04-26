namespace TradingAlpha.Domain.Interfaces;

/// <summary>
/// Unit of Work abstraction — wraps the "save" operation.
/// 
/// Keeps transaction control in the Application layer without
/// coupling to EF's DbContext. The Application layer calls
/// SaveChangesAsync() after repository operations; Infrastructure
/// implements it by delegating to DbContext.SaveChangesAsync().
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}