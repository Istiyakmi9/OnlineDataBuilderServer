using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IProductService
    {
        List<Product> ProdcutAddUpdateService(Product product, List<Files> files, IFormFileCollection fileCollection);
        List<Product> GetAllProductsService(FilterModel filterModel);
        DataSet GetProductImagesService(string FileIds);
        List<ProductCatagory> AddUpdateProductCatagoryService(ProductCatagory productCatagory);
        List<ProductCatagory> GetProductCatagoryService(FilterModel filterModel);
    }
}
