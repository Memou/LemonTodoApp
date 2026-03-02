import { createTheme } from '@mui/material';

export const theme = createTheme({
    palette: {
        mode: 'light',
        primary: { main: '#1a1a1a', light: '#333333', dark: '#000000' },
        secondary: { main: '#F7C948', light: '#FFE699', dark: '#C5A03A' },
        success: { main: '#10b981' },
        error: { main: '#ef4444' },
        warning: { main: '#F7C948' },
        background: { default: '#FFFBF5', paper: '#ffffff' },
        text: { primary: '#1a1a1a', secondary: '#666666' },
    },
    typography: {
        fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
        h1: { fontWeight: 700, fontSize: '3rem' },
        h2: { fontWeight: 700, fontSize: '2.5rem' },
        h3: { fontWeight: 600, fontSize: '2rem' },
        h4: { fontWeight: 700, fontSize: '1.75rem' },
        h5: { fontWeight: 600, fontSize: '1.5rem' },
        h6: { fontWeight: 700, fontSize: '1.25rem', letterSpacing: '-0.02em' },
        button: { textTransform: 'none', fontWeight: 600 },
    },
    shape: { borderRadius: 16 },
    components: {
        MuiButton: {
            styleOverrides: {
                root: {
                    borderRadius: 32,
                    padding: '12px 32px',
                    fontSize: '1rem',
                    fontWeight: 600,
                },
                contained: {
                    boxShadow: 'none',
                    '&:hover': { boxShadow: '0 4px 12px rgba(0,0,0,0.15)' },
                },
                containedPrimary: {
                    backgroundColor: '#F7C948',
                    color: '#1a1a1a',
                    '&:hover': { backgroundColor: '#FFD966' },
                    '&:active': { backgroundColor: '#FFE699' },
                },
            },
        },
        MuiCard: {
            styleOverrides: {
                root: {
                    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.08)',
                    borderRadius: 16,
                },
            },
        },
        MuiAppBar: {
            styleOverrides: {
                root: { boxShadow: 'none', borderBottom: '1px solid rgba(0,0,0,0.08)' },
            },
        },
    },
});
