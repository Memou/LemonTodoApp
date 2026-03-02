import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { TaskItem } from '../TaskItem';

const theme = createTheme();

describe('TaskItem', () => {
    const mockTask = {
        id: '1',
        title: 'Test Task',
        description: 'Test Description',
        priority: 1,
        isCompleted: false,
        dueDate: '2024-12-31T00:00:00Z',
        completedAt: null,
    };

    const defaultProps = {
        task: mockTask,
        onEdit: vi.fn(),
        onToggleComplete: vi.fn(),
        onDelete: vi.fn(),
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders task title', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Test Task')).toBeInTheDocument();
    });

    it('renders task description', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Test Description')).toBeInTheDocument();
    });

    it('renders priority badge', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Medium')).toBeInTheDocument();
    });

    it('renders due date', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        // Due date format: "Due 12/31/2024" or similar
        expect(screen.getByText(/Due/i)).toBeInTheDocument();
    });

    it('renders Complete button for incomplete tasks', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Complete')).toBeInTheDocument();
    });

    it('renders Undo button for completed tasks', () => {
        const completedTask = { ...mockTask, isCompleted: true, completedAt: '2024-12-25T10:00:00Z' };

        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} task={completedTask} />
            </ThemeProvider>
        );

        expect(screen.getByText('Undo')).toBeInTheDocument();
    });

    it('shows completed date badge for completed tasks', () => {
        const completedTask = { ...mockTask, isCompleted: true, completedAt: '2024-12-25T10:00:00Z' };

        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} task={completedTask} />
            </ThemeProvider>
        );

        expect(screen.getByText(/Completed/i)).toBeInTheDocument();
    });

    it('calls onEdit when task is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        const taskItem = screen.getByText('Test Task').closest('[class*="MuiBox"]');
        fireEvent.click(taskItem);

        expect(defaultProps.onEdit).toHaveBeenCalledWith(mockTask);
    });

    it('calls onToggleComplete when Complete button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        const completeButton = screen.getByText('Complete');
        fireEvent.click(completeButton);

        expect(defaultProps.onToggleComplete).toHaveBeenCalledWith(mockTask);
    });

    it('calls onEdit when Edit button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        const editButton = screen.getByText('Edit');
        fireEvent.click(editButton);

        expect(defaultProps.onEdit).toHaveBeenCalledWith(mockTask);
    });

    it('calls onDelete when Delete button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} />
            </ThemeProvider>
        );

        const deleteButton = screen.getByText('Delete');
        fireEvent.click(deleteButton);

        expect(defaultProps.onDelete).toHaveBeenCalledWith(mockTask);
    });

    it('applies strike-through style to completed task title', () => {
        const completedTask = { ...mockTask, isCompleted: true };

        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} task={completedTask} />
            </ThemeProvider>
        );

        const title = screen.getByText('Test Task');
        expect(title).toHaveStyle({ textDecoration: 'line-through' });
    });

    it('does not show due date when not provided', () => {
        const taskWithoutDueDate = { ...mockTask, dueDate: null };

        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} task={taskWithoutDueDate} />
            </ThemeProvider>
        );

        expect(screen.queryByText(/Due/i)).not.toBeInTheDocument();
    });

    it('does not show description when not provided', () => {
        const taskWithoutDescription = { ...mockTask, description: null };

        render(
            <ThemeProvider theme={theme}>
                <TaskItem {...defaultProps} task={taskWithoutDescription} />
            </ThemeProvider>
        );

        expect(screen.queryByText('Test Description')).not.toBeInTheDocument();
    });
});
