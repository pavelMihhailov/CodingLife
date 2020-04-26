﻿namespace Merchain.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Merchain.Data.Common.Repositories;
    using Merchain.Data.Models;
    using Merchain.Services.Data.Interfaces;
    using Merchain.Services.Mapping;

    public class PromoCodesService : IPromoCodesService
    {
        private readonly IRepository<PromoCode> promoCodesRepo;

        public PromoCodesService(IRepository<PromoCode> promoCodesRepo)
        {
            this.promoCodesRepo = promoCodesRepo;
        }

        public IEnumerable<PromoCode> GetAll()
        {
            return this.promoCodesRepo.All();
        }

        public IEnumerable<PromoCode> GetAllUnused()
        {
            return this.promoCodesRepo.All().Where(x => !x.IsUsed);
        }

        public async Task<PromoCode> GetByIdAsync(int id)
        {
            return await this.promoCodesRepo.GetById(id);
        }

        public IEnumerable<T> GetAllOfUserId<T>(string userId)
        {
            var promoCodes = this.promoCodesRepo.All()
                .Where(x => x.UserId == userId && !x.IsUsed)
                .To<T>().ToList();

            return promoCodes;
        }

        public async Task<Task> Delete(PromoCode promoCode)
        {
            this.promoCodesRepo.Delete(promoCode);

            await this.promoCodesRepo.SaveChangesAsync();

            return Task.CompletedTask;
        }

        public async Task GenerateNewCodes(int count, int percentageDiscount, DateTime validUntil)
        {
            for (int i = 0; i < count; i++)
            {
                await this.promoCodesRepo.AddAsync(new PromoCode
                {
                    Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                    ValidUntil = validUntil.ToUniversalTime(),
                    PercentageDiscount = percentageDiscount,
                    IsUsed = false,
                });
            }

            await this.promoCodesRepo.SaveChangesAsync();
        }
    }
}
