using Company.Domain.Entities;
using Company.Domain.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Company.Domain.Repositories.EntityFramework
{
    public class EFServiceItemsRepository : IServiceItemsRepository
    {
        private readonly AppDbContext context;
        public EFServiceItemsRepository(AppDbContext context)
        {
            this.context = context;
        }

        public void DeleteServiceItem(Guid id)
        {
            context.ServiceItem.Remove(new ServiceItem() { Id = id });
            context.SaveChanges();
        }

        public ServiceItem GetServiceItemById(Guid id)
        {
            return context.ServiceItem.FirstOrDefault(x => x.Id == id);
        }

        public IQueryable<ServiceItem> GetServiceItems()
        {
            return context.ServiceItem;
        }

        public void SaveServiceItem(ServiceItem entity)
        {
            if (entity.Id == default)
            {
                context.Entry(entity).State = EntityState.Added;
            }
            else
            {
                context.Entry(entity).State = EntityState.Modified;
            }
            context.SaveChanges();
        }
    }
}
