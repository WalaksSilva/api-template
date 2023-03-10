using System;

namespace Inova.Template.Domain.Interfaces.UoW;

public interface IUnitOfWork : IDisposable
{
    int Commit();
    void BeginTransaction();
    void BeginCommit();
    void BeginRollback();
}
