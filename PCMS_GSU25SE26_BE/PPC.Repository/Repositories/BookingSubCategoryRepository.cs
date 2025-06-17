using Microsoft.EntityFrameworkCore;
using PPC.DAO.Models;
using PPC.Repository.GenericRepository;
using PPC.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.Repository.Repositories
{
    public class BookingSubCategoryRepository : GenericRepository<BookingSubCategory>, IBookingSubCategoryRepository
    {
        public BookingSubCategoryRepository(CCPContext context) : base(context) { }

        public async Task CreateRangeAsync(List<BookingSubCategory> entities)
        {
            await _context.BookingSubCategories.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}
