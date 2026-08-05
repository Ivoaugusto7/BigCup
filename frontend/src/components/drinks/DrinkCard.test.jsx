import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { vi } from 'vitest';
import DrinkCard from './DrinkCard';

describe('DrinkCard', () => {
  const container = { id: 1, nome: 'Shot', volume: '50 ml', icone: '🥃' };

  test('renders container info and triggers APLICAR', () => {
    const onSelect = vi.fn();
    render(<DrinkCard container={container} isSelected={false} onSelect={onSelect} />);

    expect(screen.getByText('Shot')).toBeInTheDocument();
    expect(screen.getByText('50 ml')).toBeInTheDocument();

    const aplicar = screen.getByText('APLICAR');
    fireEvent.click(aplicar);
    expect(onSelect).toHaveBeenCalledWith(container);
  });

  test('shows REMOVER when selected and triggers removal', () => {
    const onSelect = vi.fn();
    render(<DrinkCard container={container} isSelected={true} onSelect={onSelect} />);

    const remover = screen.getByText('REMOVER');
    fireEvent.click(remover);
    expect(onSelect).toHaveBeenCalledWith(container);
  });
});
