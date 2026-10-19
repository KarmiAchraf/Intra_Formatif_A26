using Microsoft.AspNetCore.SignalR;
using SignalR.Services;

namespace SignalR.Hubs
{
    public class PizzaHub : Hub
    {
        private readonly PizzaManager _pizzaManager;

        public PizzaHub(PizzaManager pizzaManager) {
            _pizzaManager = pizzaManager;
        }

        public override async Task OnConnectedAsync()
        {
            _pizzaManager.AddUser();
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("UpdateNbUsers", _pizzaManager.NbConnectedUsers);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _pizzaManager.RemoveUser();
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("UpdateNbUsers", _pizzaManager.NbConnectedUsers);
        }

        public async Task SelectChoice(PizzaChoice choice)
        {
            string groupName = _pizzaManager.GetGroupName(choice);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            int price = _pizzaManager.GetPizzaPrice(choice);
            int money = _pizzaManager.GetMoney(choice);
            int nbPizzas = _pizzaManager.GetNbPizzas(choice);

            await Clients.Group(groupName).SendAsync("UpdatePizzaPrice", price);
            await Clients.Group(groupName).SendAsync("UpdateNbPizzasAndMoney", nbPizzas, money);
        }

        public async Task UnselectChoice(PizzaChoice choice)
        {
            string groupName = _pizzaManager.GetGroupName(choice);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task AddMoney(PizzaChoice choice)
        {
            _pizzaManager.IncreaseMoney(choice);     

            string groupName = _pizzaManager.GetGroupName(choice);
            int money = _pizzaManager.GetMoney(choice);

            await Clients.Group(groupName).SendAsync("UpdateMoney", money);
        }

        public async Task BuyPizza(PizzaChoice choice)
        {
            _pizzaManager.BuyPizza(choice);
            string groupName = _pizzaManager.GetGroupName(choice);
            int money = _pizzaManager.GetMoney(choice);
            int nbPizzas = _pizzaManager.GetNbPizzas(choice);

            await Clients.Group(groupName).SendAsync("UpdateNbPizzasAndMoney", nbPizzas, money);
        }
    }
}
