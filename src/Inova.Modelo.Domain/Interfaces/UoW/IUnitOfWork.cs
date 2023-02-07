using System;

namespace Inova.Modelo.Domain.Interfaces.UoW;

public interface IUnitOfWork : IDisposable
{
    int Commit();
    void BeginTransaction();
    void BeginCommit();
    void BeginRollback();
}
