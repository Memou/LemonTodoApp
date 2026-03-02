import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { TaskFilters } from '../TaskFilters';

const theme = createTheme();

describe('TaskFilters', () => {
    const defaultProps = {
        filter: 'all',
        setFilter: vi.fn(),
        sortBy: 'createdAt',
        setSortBy: vi.fn(),
        sortDirection: 'desc',
        setSortDirection: vi.fn(),
        onCreateTask: vi.fn(),
        hasActiveTasks: true,
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders Your Tasks title', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskFilters {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Your Tasks')).toBeInTheDocument();
    });

    it('renders New Task button when tasks exist', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskFilters {...defaultProps} hasActiveTasks={true} />
            </ThemeProvider>
        );

        // Should find at least one "New Task" button (mobile or desktop)
        const newTaskButtons = screen.getAllByText('New Task');
        expect(newTaskButtons.length).toBeGreaterThan(0);
    });

    it('does not render New Task button when no tasks', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskFilters {...defaultProps} hasActiveTasks={false} />
            </ThemeProvider>
        );

        expect(screen.queryByText('New Task')).not.toBeInTheDocument();
    });

    it('calls onCreateTask when New Task button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskFilters {...defaultProps} />
            </ThemeProvider>
        );

        const newTaskButton = screen.getAllByText('New Task')[0];
        fireEvent.click(newTaskButton);

        expect(defaultProps.onCreateTask).toHaveBeenCalledTimes(1);
    });

    it('renders filter dropdown with correct value', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskFilters {...defaultProps} />
            </ThemeProvider>
        );

        // The select shows "All Tasks" as the selected text
        expect(screen.getByText('All Tasks')).toBeInTheDocument();
    });

    it('renders sort by dropdown with correct value', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskFilters {...defaultProps} />
            </ThemeProvider>
        );

        // The select shows "Date Created" as the selected text
        expect(screen.getByText('Date Created')).toBeInTheDocument();
    });

    it('renders sort direction button', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskFilters {...defaultProps} />
            </ThemeProvider>
        );

        // Should have an arrow icon button
        const buttons = screen.getAllByRole('button');
        expect(buttons.length).toBeGreaterThan(0);
    });

    it('calls setSortDirection when sort direction button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskFilters {...defaultProps} />
            </ThemeProvider>
        );

        // Find the icon button (last button typically)
        const buttons = screen.getAllByRole('button');
        const sortButton = buttons[buttons.length - 1];
        
        fireEvent.click(sortButton);

        expect(defaultProps.setSortDirection).toHaveBeenCalled();
    });
});
