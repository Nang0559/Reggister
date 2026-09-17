using FVN_REGISTER.Core.Enums;

namespace FVN_REGISTER.Application.Interfaces.Common;

public interface IHrmChangeLogStrategy<TKey, TChange, TEntity>
{
    TKey GetKey(TChange change);
    TKey GetEntityKey(TEntity entity);
    HrmChangeAction GetAction(TChange change);
    TEntity MapToNewEntity(TChange change);
    void ApplyUpdate(TEntity entity, TChange change);
    void ApplyDelete(TEntity entity);
}
