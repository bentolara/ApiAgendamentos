using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public class AgendamentoService : IAgendamentoService
{
    private readonly IAgendamentoRepository _repository;
    private readonly IMapper _mapper;

    public AgendamentoService(IAgendamentoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AgendamentoDto> CriarAsync(CriarAgendamentoDto dto)
    {
        var agendamento = _mapper.Map<Agendamento>(dto);
        agendamento.Id = Guid.NewGuid();
        agendamento.DataCriacao = DateTime.UtcNow;

        var existeConflito = await _repository.ExisteConflitoHorarioAsync(
            agendamento.DataHoraInicio,
            agendamento.DataHoraFim);

        if (existeConflito)
        {
            throw new DomainException("Já existe um agendamento neste horário.");
        }

        var resultado = await _repository.AddAsync(agendamento);
        return _mapper.Map<AgendamentoDto>(resultado);
    }

    public async Task<IEnumerable<AgendamentoDto>> ObterTodosAsync()
    {
        var agendamentos = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<AgendamentoDto>>(agendamentos);
    }

    public async Task<AgendamentoDto?> ObterPorIdAsync(Guid id)
    {
        var agendamento = await _repository.GetByIdAsync(id);
        return agendamento == null ? null : _mapper.Map<AgendamentoDto>(agendamento);
    }

    public async Task<AgendamentoDto> AtualizarAsync(Guid id, AtualizarAgendamentoDto dto)
    {
        var agendamento = await _repository.GetByIdAsync(id);
        if (agendamento == null)
        {
            throw new DomainException("Agendamento não encontrado.");
        }

        var existeConflito = await _repository.ExisteConflitoHorarioAsync(
            dto.DataHoraInicio,
            dto.DataHoraFim,
            id);

        if (existeConflito)
        {
            throw new DomainException("Já existe um agendamento neste horário.");
        }

        _mapper.Map(dto, agendamento);
        agendamento.DataAtualizacao = DateTime.UtcNow;

        var resultado = await _repository.UpdateAsync(agendamento);
        return _mapper.Map<AgendamentoDto>(resultado);
    }

    public async Task<bool> DeletarAsync(Guid id)
    {
        var existe = await _repository.ExistsAsync(id);
        if (!existe)
        {
            throw new DomainException("Agendamento não encontrado.");
        }

        return await _repository.DeleteAsync(id);
    }
}

