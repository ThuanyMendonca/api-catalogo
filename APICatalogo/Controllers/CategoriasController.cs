using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public CategoriasController(IUnitOfWork context, IConfiguration configuration, ILogger<CategoriasController> logger, IMapper mapper)
        {
            _uof = context;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("autor")]
        public string GetAutor()
        {
            var autor = _configuration["autor"];
            var conexao = _configuration["ConnectionStrings:DefaultConnection"];
            return $"Autor: {autor} e conexão: {conexao}";

        }

        [HttpGet("produtos")]
        public ActionResult<IEnumerable<CategoriaDTO>> GetCategoriasProdutos()
        {
            _logger.LogInformation("###### GET api/categorias/produtos #####");
            var categoria = _uof.CategoriaRepository.GetCategoriasProdutos().ToList();

            var categoriaDto = _mapper.Map<List<CategoriaDTO>>(categoria);
            return categoriaDto;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CategoriaDTO>> Get([FromQuery] CategoriasParameters categoriasParameters)
        {
            var categorias = _uof.CategoriaRepository.GetCategorias(categoriasParameters);
            if(categorias is null)
            {
                return NotFound("Categoria não encontrada");
            }

            var metadata = new
            {
                categorias.TotalCount,
                categorias.PageSize,
                categorias.CurrentPage,
                categorias.TotalPages,
                categorias.HasNext,
                categorias.HasPrevious
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));

            var categoriasDto = _mapper.Map<List<CategoriaDTO>>(categorias);

            return Ok(categoriasDto);
        }

        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<CategoriaDTO> Get(int id) 
        {
            Categoria categoria = _uof.CategoriaRepository.GetById(c => c.Id == id);

            if (categoria is null)
            {
                return NotFound();
            }

            var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);

            return Ok(categoriaDto);
        }

        [HttpPost]
        public ActionResult Post(CategoriaDTO categoriaDTO) 
        {
            var categoria = _mapper.Map<Categoria>(categoriaDTO);
            if(categoria is null)
            {
                return BadRequest();
            }
            _uof.CategoriaRepository.Add(categoria);
            _uof.Commit();

            var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);

            return new CreatedAtRouteResult("ObterCategoria", new {id = categoriaDto.Id, categoriaDto });
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, CategoriaDTO categoriaDto) 
        {
            if(id != categoriaDto.Id)
            {
                return BadRequest();
            }

            var categoria = _mapper.Map<Categoria>(categoriaDto);
            _uof.CategoriaRepository.Update(categoria);
            _uof.Commit();

            var categoriaDTO = _mapper.Map<CategoriaDTO>(categoria);
            return Ok(categoriaDTO);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id) 
        {
            var categoria = _uof.CategoriaRepository.GetById(c => c.Id == id);

            if(categoria == null) 
            { 
                return NotFound(); 
            }

            _uof.CategoriaRepository.Delete(categoria);
            _uof.Commit();

            return Ok();
        }
    }
}
