using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace APICatalogo.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    [EnableCors("PermitirApiRequest")]
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
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasProdutos()
        {
            _logger.LogInformation("###### GET api/categorias/produtos #####");
            var categoria = await _uof.CategoriaRepository.GetCategoriasProdutos();

            var categoriaDto = _mapper.Map<List<CategoriaDTO>>(categoria);
            return categoriaDto;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery] CategoriasParameters categoriasParameters)
        {
            var categorias = await _uof.CategoriaRepository.GetCategorias(categoriasParameters);
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
        public async Task<ActionResult<CategoriaDTO>> Get(int id) 
        {
            Categoria categoria = await _uof.CategoriaRepository.GetById(c => c.Id == id);

            if (categoria is null)
            {
                return NotFound();
            }

            var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);

            return Ok(categoriaDto);
        }

        [HttpPost]
        public async Task<ActionResult> Post(CategoriaDTO categoriaDTO) 
        {
            var categoria = _mapper.Map<Categoria>(categoriaDTO);
            if(categoria is null)
            {
                return BadRequest();
            }
            _uof.CategoriaRepository.Add(categoria);
            await _uof.Commit();

            var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);

            return new CreatedAtRouteResult("ObterCategoria", new {id = categoriaDto.Id, categoriaDto });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, CategoriaDTO categoriaDto) 
        {
            if(id != categoriaDto.Id)
            {
                return BadRequest();
            }

            var categoria = _mapper.Map<Categoria>(categoriaDto);
            _uof.CategoriaRepository.Update(categoria);
            await _uof.Commit();

            var categoriaDTO = _mapper.Map<CategoriaDTO>(categoria);
            return Ok(categoriaDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id) 
        {
            var categoria = await _uof.CategoriaRepository.GetById(c => c.Id == id);

            if(categoria == null) 
            { 
                return NotFound(); 
            }

            _uof.CategoriaRepository.Delete(categoria);
            await _uof.Commit();

            return Ok();
        }
    }
}
