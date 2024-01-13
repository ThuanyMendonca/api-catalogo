using APICatalogo.Context;
using APICatalogo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("produtos")]
        public ActionResult<IEnumerable<Categoria>> GetCategoriasProdutos()
        {
            //return _context.Categorias.Include(p=> p.Produtos).ToList();
            return _context.Categorias.Include(p => p.Produtos).Where(c => c.Id <= 5).ToList(); // otimizado
        }

        [HttpGet]
        public ActionResult<IEnumerable<Categoria>> Get()
        {
            var categorias = _context.Categorias.AsNoTracking().ToList();
            if(categorias is null)
            {
                return NotFound("Categoria não encontrada");
            }

            return Ok(categorias);
        }

        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<Categoria> Get(int id) 
        {
            Categoria categoria = _context.Categorias.FirstOrDefault(c => c.Id == id);

            if (categoria is null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        [HttpPost]
        public ActionResult Post(Categoria categoria) 
        {
            if(categoria is null)
            {
                return BadRequest();
            }
            _context.Categorias.Add(categoria);
            _context.SaveChanges();

            return new CreatedAtRouteResult("ObterCategoria", new {id = categoria.Id, categoria});
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Categoria categoria) 
        {
            if(id != categoria.Id)
            {
                return BadRequest();
            }

            _context.Entry(categoria).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok(categoria);
        }

        [HttpDelete("{id:int}")]
        public ActionResult<Categoria> Delete(int id) 
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.Id == id);

            if(categoria == null) 
            { 
                return NotFound(); 
            }

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();
            return Ok();
        }
    }
}
