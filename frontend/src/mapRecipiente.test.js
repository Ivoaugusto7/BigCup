import { mapRecipiente } from './App';

describe('mapRecipiente', () => {
  test('usa capacidadeMl vindo da API', () => {
    expect(mapRecipiente({ id: 1, nome: 'Shot', capacidadeMl: 50 })).toEqual({
      id: 1,
      nome: 'Shot',
      volume: '50 ml',
      icone: '🥃',
    });
  });

  test('converte capacidades a partir de 1000 ml para litros', () => {
    expect(mapRecipiente({ id: 5, nome: 'Garrafa', capacidadeMl: 2000 })).toMatchObject({
      volume: '2 Litros',
      icone: '🍾',
    });
    expect(mapRecipiente({ id: 6, nome: 'Galão', capacidadeMl: 5000 })).toMatchObject({
      volume: '5 Litros',
      icone: '🛢️',
    });
  });

  test('copos recebem ícone conforme a capacidade', () => {
    expect(mapRecipiente({ id: 2, nome: 'Copo', capacidadeMl: 300 }).icone).toBe('🥛');
    expect(mapRecipiente({ id: 3, nome: 'Copo', capacidadeMl: 500 }).icone).toBe('🍺');
    expect(mapRecipiente({ id: 4, nome: 'Copo', capacidadeMl: 700 }).volume).toBe('700 ml');
  });
});
