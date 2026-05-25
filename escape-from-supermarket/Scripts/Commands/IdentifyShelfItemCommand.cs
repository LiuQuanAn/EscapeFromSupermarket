using EscapeFromSupermarket.Events;
using EscapeFromSupermarket.Models;
using QFramework;

namespace EscapeFromSupermarket.Commands
{
    public class IdentifyShelfItemCommand : AbstractCommand
    {
        private readonly int _shelfId;
        private readonly int _instanceId;

        public IdentifyShelfItemCommand(int shelfId, int instanceId)
        {
            _shelfId = shelfId;
            _instanceId = instanceId;
        }

        protected override void OnExecute()
        {
            var shelf = this.GetModel<ShelfModel>();
            shelf.MarkItemIdentified(_shelfId, _instanceId);
            this.SendEvent(new ShelfChangedEvent(_shelfId));
        }
    }
}
