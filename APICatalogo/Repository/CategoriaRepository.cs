using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repository
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext context) : base(context)
        {
        }

        public IEnumerable<Categoria> GetCategoriasProdutos()
        {
            return Get().Include(x => x.Produtos);
        }

        public PagedList<Categoria> GetCategorias(CategoriasParameters categoriasParameters)
        {
            //return Get()
            //    .OrderBy(on => on.Nome)
            //    .Skip((produtosParameters.PageNumber - 1) * produtosParameters.PageSize)
            //    .Take(produtosParameters.PageSize)
            //    .ToList();

            return PagedList<Categoria>.ToPagedList(Get().OrderBy(on => on.Id), categoriasParameters.PageNumber, categoriasParameters.PageSize);
        }

    }
}
