using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly ApplicationDbContext _context;

    public AgendamentoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Agendamento?> GetByIdAsync(Guid id)
    {
        return await _context.Agendamentos.FindAsync(id);
    }

    public async Task<IEnumerable<Agendamento>> GetAllAsync()
    {
        return await _context.Agendamentos.ToListAsync();
    }

    public async Task<Agendamento> AddAsync(Agendamento entity)
    {
        await _context.Agendamentos.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Agendamento> UpdateAsync(Agendamento entity)
    {
        _context.Agendamentos.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Agendamentos.FindAsync(id);
        if (entity == null)
            return false;

        _context.Agendamentos.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Agendamentos.AnyAsync(e => e.Id == id);
    }

    public async Task<bool> ExisteConflitoHorarioAsync(DateTime dataHoraInicio, DateTime dataHoraFim, Guid? idExcluir = null)
    {
        var query = _context.Agendamentos.Where(a =>
            (dataHoraInicio >= a.DataHoraInicio && dataHoraInicio < a.DataHoraFim) ||
            (dataHoraFim > a.DataHoraInicio && dataHoraFim <= a.DataHoraFim) ||
            (dataHoraInicio <= a.DataHoraInicio && dataHoraFim >= a.DataHoraFim));

        if (idExcluir.HasValue)
        {
            query = query.Where(a => a.Id != idExcluir.Value);
        }

        return await query.AnyAsync();
    }
}

