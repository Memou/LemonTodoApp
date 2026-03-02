import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { EmptyState } from '../EmptyState';

const theme = createTheme();

describe('EmptyState', () => {
    it('renders no tasks yet message', () => {
        const onCreateTask = vi.fn();

        render(
            <ThemeProvider theme={theme}>
                <EmptyState onCreateTask={onCreateTask} />
            </ThemeProvider>
        );

        expect(screen.getByText('No tasks yet')).toBeInTheDocument();
        expect(screen.getByText('Create your first task to get started')).toBeInTheDocument();
    });

    it('renders create button', () => {
        const onCreateTask = vi.fn();

        render(
            <ThemeProvider theme={theme}>
                <EmptyState onCreateTask={onCreateTask} />
            </ThemeProvider>
        );

        expect(screen.getByText('Create Your First Task')).toBeInTheDocument();
    });

    it('calls onCreateTask when button is clicked', () => {
        const onCreateTask = vi.fn();

        render(
            <ThemeProvider theme={theme}>
                <EmptyState onCreateTask={onCreateTask} />
            </ThemeProvider>
        );

        const createButton = screen.getByText('Create Your First Task');
        fireEvent.click(createButton);

        expect(onCreateTask).toHaveBeenCalledTimes(1);
    });
});
