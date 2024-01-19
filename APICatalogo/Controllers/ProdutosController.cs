using APICatalogo.Context;
using APICatalogo.Filters;
using APICatalogo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<IEnumerable<Produto>>> Get()
        {
            var produtos = await _context.Produtos.AsNoTracking().ToListAsync();
            if(produtos is null)
            {
                return NotFound();
            }

            return produtos;
        }

        [HttpGet("{id:int}", Name = "ObterProduto")]
        public ActionResult<Produto> Get(int id)
        {
            //throw new Exception("Exception ao retornar produto pelo id");
            
            var produto = _context.Produtos.FirstOrDefault(p => p.Id == id);
            if (produto is null)
            {
                return NotFound();
            }

            return produto;
        }

        [HttpPost]
        public ActionResult Post(Produto produto)
        {
            if (produto is null)
            {
                return BadRequest();
            }
            _context.Produtos.Add(produto); // inclui apenas no contexto em memória
            _context.SaveChanges(); // persiste os dados no banco de dados

            /*
             * retorna 201 created
             * aciona a rota ObterProduto com o id do produto
             * retorna um id do produto criado, adiciona o id no cabeçalho location 
             * e especificar a uri para obter o produto criado
            */
            return new CreatedAtRouteResult("ObterProduto", new { id = produto.Id }, produto);
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Produto produto)
        {
            if (id != produto.Id)
            {
                return BadRequest();
            }

            _context.Entry(produto).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok(produto);
        }

        [HttpPatch("{id:int}")]
        public ActionResult Patch(int id, Produto produto)
        {
            if (id != produto.Id)
            {
                return BadRequest();
            }

            _context.Entry(produto).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok(produto);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id) 
        {
            Produto produto = _context.Produtos.FirstOrDefault(p => p.Id == id);
            /* 
             * alternativa é o Find que busca primeiro na memória e caso não encontre, ele vai ao banco de dados
             * porém o parâmetro deve ser primary key
            */
            //var produto = _context.Produtos.Find(id);
            
            if(produto is null)
            {
                return NotFound("Produto não localizado");
            }

            _context.Produtos.Remove(produto);
            _context.SaveChanges();

            return Ok();
        }

    }
}
