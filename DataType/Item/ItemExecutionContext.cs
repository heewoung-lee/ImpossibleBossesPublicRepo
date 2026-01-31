using Controller;

namespace DataType.Item
{
    public sealed class ItemExecutionContext : ExecutionContext
    {
        public ItemDataSO ItemData => (ItemDataSO)Data;
        public ItemExecutionContext(BaseController caster, ItemDataSO data) : base(caster, data) {}
    }
}