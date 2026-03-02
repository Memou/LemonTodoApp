import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { LoadingScreen } from '../LoadingScreen';

const theme = createTheme();

describe('LoadingScreen', () => {
    it('renders loading screen with taskflow title', () => {
        render(
            <ThemeProvider theme={theme}>
                <LoadingScreen />
            </ThemeProvider>
        );

        expect(screen.getByText('taskflow')).toBeInTheDocument();
    });

    it('displays loading spinner', () => {
        render(
            <ThemeProvider theme={theme}>
                <LoadingScreen />
            </ThemeProvider>
        );

        const progressBar = screen.getByRole('progressbar');
        expect(progressBar).toBeInTheDocument();
    });

    it('applies correct styling', () => {
        const { container } = render(
            <ThemeProvider theme={theme}>
                <LoadingScreen />
            </ThemeProvider>
        );

        const mainBox = container.querySelector('[class*="MuiBox"]');
        expect(mainBox).toBeInTheDocument();
    });
});
