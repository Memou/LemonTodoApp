import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { TaskList } from '../TaskList';

const theme = createTheme();

describe('TaskList', () => {
    const mockTasks = [
        {
            id: '1',
            title: 'Task 1',
            description: 'Description 1',
            priority: 1,
            isCompleted: false,
            dueDate: '2024-12-31T00:00:00Z',
        },
        {
            id: '2',
            title: 'Task 2',
            description: 'Description 2',
            priority: 2,
            isCompleted: true,
            completedAt: '2024-12-25T00:00:00Z',
        },
    ];

    const defaultProps = {
        tasks: mockTasks,
        onEdit: vi.fn(),
        onToggleComplete: vi.fn(),
        onDelete: vi.fn(),
    };

    it('renders all tasks', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskList {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Task 1')).toBeInTheDocument();
        expect(screen.getByText('Task 2')).toBeInTheDocument();
    });

    it('renders empty list when no tasks provided', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskList {...defaultProps} tasks={[]} />
            </ThemeProvider>
        );

        expect(screen.queryByText('Task 1')).not.toBeInTheDocument();
        expect(screen.queryByText('Task 2')).not.toBeInTheDocument();
    });

    it('passes correct props to TaskItem components', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskList {...defaultProps} />
            </ThemeProvider>
        );

        // Check that all task items are rendered with correct data
        expect(screen.getByText('Description 1')).toBeInTheDocument();
        expect(screen.getByText('Description 2')).toBeInTheDocument();
    });

    it('renders dividers between tasks', () => {
        const { container } = render(
            <ThemeProvider theme={theme}>
                <TaskList {...defaultProps} />
            </ThemeProvider>
        );

        // Should have dividers (count should be tasks.length - 1)
        const dividers = container.querySelectorAll('hr');
        expect(dividers.length).toBeGreaterThan(0);
    });
});
