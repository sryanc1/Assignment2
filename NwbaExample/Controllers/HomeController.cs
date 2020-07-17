using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NwbaExample.Data;
using NwbaExample.Models;
using NwbaExample.Utilities;
using NwbaExample.ViewModels;

namespace NwbaExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly NwbaContext _context;

        // Simulate being "logged in" as Matthew Bolger by hard-coding the CustomerID.
        private const int _customerID = 2100;

        public HomeController(NwbaContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            // Lazy loading.
            //var customer = await _context.Customers.FindAsync(_customerID);

            // Eager loading.
            var customer = await _context.Customers.Include(x => x.Accounts).
                FirstOrDefaultAsync(x => x.CustomerID == _customerID);

            return View(customer);
        }

        public async Task<IActionResult> Deposit(int accountNumber)
        {
            return View(
                new DepositViewModel
                {
                    AccountNumber = accountNumber,
                    Account = await _context.Accounts.FindAsync(accountNumber)
                });
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel viewModel)
        {
            viewModel.Account = await _context.Accounts.FindAsync(viewModel.AccountNumber);

            if(viewModel.Amount <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must be positive.");
                return View(viewModel);
            }
            if(viewModel.Amount.HasMoreThanTwoDecimalPlaces())
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount cannot have more than 2 decimal places.");
                return View(viewModel);
            }

            // Note this code could be moved out of the controller, e.g., into the model or repository (design pattern).
            viewModel.Account.Balance += viewModel.Amount;
            viewModel.Account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    Amount = viewModel.Amount,
                    TransactionTimeUtc = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
