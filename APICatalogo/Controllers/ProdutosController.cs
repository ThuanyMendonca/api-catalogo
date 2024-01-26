using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;

        public ProdutosController(IUnitOfWork context, IMapper mapper)
        {
            _uof = context;
            _mapper = mapper;
        }

        [HttpGet("menorpreco")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosPreco()
        {
            var produtos = await _uof.ProdutoRepository.GetProdutosPorPreco();
            List<ProdutoDTO> produtosDto = _mapper.Map<List<ProdutoDTO>>(produtos);

            return produtosDto;
        }

        [HttpGet]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get([FromQuery] ProdutosParameters produtosParameters)
        {
            var produtos = await _uof.ProdutoRepository.GetProdutos(produtosParameters);
 
            if (produtos is null)
            {
                return NotFound();
            }


            var metadata = new
            {
                produtos.TotalCount,
                produtos.PageSize,
                produtos.CurrentPage,
                produtos.TotalPages,
                produtos.HasNext,
                produtos.HasPrevious
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));

            var produtosDto = _mapper.Map<List<ProdutoDTO>>(produtos);
            return produtosDto;
        }

        [HttpGet("{id:int}", Name = "ObterProduto")]
        public async Task<ActionResult<ProdutoDTO>> Get(int id)
        {
            //throw new Exception("Exception ao retornar produto pelo id");
            
            Produto produto = await _uof.ProdutoRepository.GetById(p => p.Id == id);
            if (produto is null)
            {
                return NotFound();
            }

            ProdutoDTO produtosDto = _mapper.Map<ProdutoDTO>(produto);

            return produtosDto;
        }

        [HttpPost]
        public async Task<ActionResult> Post(ProdutoDTO produtoDto)
        {
            // transforma os dados da DTO em Produto Model
            var produto = _mapper.Map<Produto>(produtoDto);

            _uof.ProdutoRepository.Add(produto); // inclui apenas no contexto em memória
            await _uof.Commit(); // persiste os dados no banco de dados

            /*
             * retorna 201 created
             * aciona a rota ObterProduto com o id do produto
             * retorna um id do produto criado, adiciona o id no cabeçalho location 
             * e especificar a uri para obter o produto criado
            */

            // transforma o Produto Model em ProdutoDTO
            var produtoDTO = _mapper.Map<ProdutoDTO>(produto);
            return new CreatedAtRouteResult("ObterProduto", new { id = produto.Id }, produtoDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ProdutoDTO produtoDto)
        {
            if (id != produtoDto.Id)
            {
                return BadRequest();
            }

            //_uof.Entry(produto).State = EntityState.Modified;
            //_uof.SaveChanges();

            Produto produto = _mapper.Map<Produto>(produtoDto);

            _uof.ProdutoRepository.Update(produto);
            await _uof.Commit();
            return Ok();
        }       

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ProdutoDTO>> Delete(int id) 
        {
            Produto produto = await _uof.ProdutoRepository.GetById(p => p.Id == id);
            /* 
             * alternativa é o Find que busca primeiro na memória e caso não encontre, ele vai ao banco de dados
             * porém o parâmetro deve ser primary key
            */
            //var produto = _uof.Produtos.Find(id);
            
            if(produto is null)
            {
                return NotFound("Produto não localizado");
            }

            _uof.ProdutoRepository.Delete(produto);
            await _uof.Commit();

            ProdutoDTO produtoDto = _mapper.Map<ProdutoDTO>(produto);

            return Ok(produtoDto);
        }

    }
}
