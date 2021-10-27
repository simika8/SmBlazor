using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public static class ProductODataEdmModel
    {
        public static Microsoft.OData.Edm.IEdmModel GetEdmModel()
        {
            var builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();


            builder.EntitySet<Models.Product>(nameof(Models.Product));
            builder.EntitySet<Models.ProductExt>(nameof(Models.ProductExt));
            builder.EntitySet<Models.InventoryStock>(nameof(Models.InventoryStock));

            return builder.GetEdmModel();

        }


    }
}
