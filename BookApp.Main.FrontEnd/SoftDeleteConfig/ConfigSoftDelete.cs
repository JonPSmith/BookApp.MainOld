// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using ModMon.Books.Domain.SupportTypes;
using ModMon.Books.Persistence;
using SoftDeleteServices.Configuration;

namespace ModMon.UI.SoftDeleteConfig
{
    public class ConfigSoftDelete : SingleSoftDeleteConfiguration<ISoftDelete>
    {

        public ConfigSoftDelete(BookDbContext context)
            : base(context)
        {
            GetSoftDeleteValue = entity => entity.SoftDeleted;
            SetSoftDeleteValue = (entity, value) => entity.AlterSoftDelete(value);

        }
    }
}