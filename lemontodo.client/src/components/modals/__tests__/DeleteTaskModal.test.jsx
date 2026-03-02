import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { DeleteTaskModal } from '../DeleteTaskModal';

const theme = createTheme();

describe('DeleteTaskModal', () => {
    const defaultProps = {
        open: true,
        taskTitle: 'Task to Delete',
        onClose: vi.fn(),
        onConfirm: vi.fn(),
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders modal when open', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Delete Task')).toBeInTheDocument();
    });

    it('does not render when closed', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} open={false} />
            </ThemeProvider>
        );

        expect(screen.queryByText('Delete Task')).not.toBeInTheDocument();
    });

    it('displays confirmation message', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Are you sure you want to delete this task?')).toBeInTheDocument();
    });

    it('displays task title', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('"Task to Delete"')).toBeInTheDocument();
    });

    it('renders Cancel and Delete buttons', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Cancel')).toBeInTheDocument();
        expect(screen.getByText('Delete')).toBeInTheDocument();
    });

    it('calls onClose when Cancel button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const cancelButton = screen.getByText('Cancel');
        fireEvent.click(cancelButton);

        expect(defaultProps.onClose).toHaveBeenCalledTimes(1);
    });

    it('calls onConfirm when Delete button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const deleteButton = screen.getByText('Delete');
        fireEvent.click(deleteButton);

        expect(defaultProps.onConfirm).toHaveBeenCalledTimes(1);
    });

    it('Delete button has error color', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const deleteButton = screen.getByText('Delete');
        expect(deleteButton).toHaveClass('MuiButton-colorError');
    });

    it('handles empty task title', () => {
        render(
            <ThemeProvider theme={theme}>
                <DeleteTaskModal {...defaultProps} taskTitle="" />
            </ThemeProvider>
        );

        expect(screen.getByText('""')).toBeInTheDocument();
    });
});
