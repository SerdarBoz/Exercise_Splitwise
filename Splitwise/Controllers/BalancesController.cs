using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Splitwise.Models;

namespace Splitwise.Controllers
{
    public class BalancesController : Controller
    {
        private readonly ApplicationDbContext context;
        private static List<Balance>balances = new List<Balance>();

        public BalancesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: Balances
        public async Task<IActionResult> Index()
        {
            return View(await context.Balances.ToListAsync());
        }

        // GET: Balances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var balance = await context.Balances
                .Include(b => b.Persons)
                    .ThenInclude(p => p.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (balance == null)
            {
                return NotFound();
            }
            double totalPayments = balance.Persons.SelectMany(p => p.Payments).Sum(p => p.Amount);
            double averagePaymentPerPerson = totalPayments / balance.Persons.Count;
            foreach (var person in balance.Persons)
            {
                person.BalanceTotal = Math.Round(person.Payments.Sum(p => p.Amount) - averagePaymentPerPerson, 2);
            }

            return View(balance);
        }

        public async Task<IActionResult> DetailsPayments(int id)
        {
            var person = await context.Persons
                .Include(p => p.Payments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }


        // GET: Balances/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Balances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Balance balance)
        {
            if (ModelState.IsValid)
            {
                context.Add(balance);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(balance);
        }
        [HttpGet]
        [ActionName("AddPerson")]
        public IActionResult AddPerson(int balanceId)
        {
            var balance = context.Balances.Find(balanceId);
            if (balance == null)
            {
                return NotFound();
            }

            ViewBag.BalanceId = balanceId;
            return View();
        }

        [HttpPost]
        [ActionName("AddPerson")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPerson([Bind("Id,Name,BalanceTotal")] Person person, int balanceId)
        {
            var balance = await context.Balances.FindAsync(balanceId);
            if (balance == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                person.BalanceId = balanceId;
                context.Persons.Add(person);
                await context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = balanceId });
            }
            ViewBag.BalanceId = balanceId;
            return View(person);
        }

        [HttpGet]
        public IActionResult AddPayment(int personId)
        {
            var person = context.Persons.Find(personId);
            if (person == null)
            {
                return NotFound();
            }

            ViewBag.PersonId = personId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment([Bind("Id,PersonId,Description,Amount")] Payment payment, int personId)
        {
            var person = await context.Persons.FindAsync(personId);
            if (person == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                payment.PersonId = personId;
                context.Payments.Add(payment);
                await context.SaveChangesAsync();

                var totalPayments = await context.Payments
                    .Where(p => p.PersonId == personId)
                    .SumAsync(p => p.Amount);

                person.BalanceTotal = totalPayments;
                context.Persons.Update(person);
                await context.SaveChangesAsync();

                return RedirectToAction("Details", "Balances", new { id = person.BalanceId });
            }

            ViewBag.PersonId = personId;
            return View(payment);
        }



        // GET: Balances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var balance = await context.Balances.FindAsync(id);
            if (balance == null)
            {
                return NotFound();
            }
            return View(balance);
        }

        // POST: Balances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Balance balance)
        {
            if (id != balance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(balance);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BalanceExists(balance.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(balance);
        }

        // GET: Balances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var balance = await context.Balances
                .FirstOrDefaultAsync(m => m.Id == id);
            if (balance == null)
            {
                return NotFound();
            }

            return View(balance);
        }

        // POST: Balances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var balance = await context.Balances.FindAsync(id);
            if (balance != null)
            {
                context.Balances.Remove(balance);
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(int paymentId, int personId)
        {
            var payment = await context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return NotFound();
            }

            context.Payments.Remove(payment);
            await context.SaveChangesAsync();

            // Recalculate the BalanceTotal for the person
            var person = await context.Persons
                .Include(p => p.Payments)
                .FirstOrDefaultAsync(p => p.Id == personId);

            if (person != null)
            {
                person.BalanceTotal = person.Payments.Sum(p => p.Amount);
                context.Persons.Update(person);
                await context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(DetailsPayments), new { id = personId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePerson(int personId, int balanceId)
        {
            var person = await context.Persons
                .Include(p => p.Payments)
                .FirstOrDefaultAsync(p => p.Id == personId);

            if (person == null)
            {
                return NotFound();
            }

            context.Persons.Remove(person);
            await context.SaveChangesAsync();

            // Update the balance details page to reflect changes
            var balance = await context.Balances
                .Include(b => b.Persons)
                .ThenInclude(p => p.Payments)
                .FirstOrDefaultAsync(b => b.Id == balanceId);

            if (balance == null)
            {
                return NotFound();
            }

            // Recalculate BalanceTotal for each person
            double totalPayments = balance.Persons.SelectMany(p => p.Payments).Sum(p => p.Amount);
            double averagePaymentPerPerson = totalPayments / balance.Persons.Count;

            foreach (var remainingPerson in balance.Persons)
            {
                remainingPerson.BalanceTotal = Math.Round(remainingPerson.Payments.Sum(p => p.Amount) - averagePaymentPerPerson, 2);
            }

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = balanceId });
        }

        private bool BalanceExists(int id)
        {
            return context.Balances.Any(e => e.Id == id);
        }
    }
}
