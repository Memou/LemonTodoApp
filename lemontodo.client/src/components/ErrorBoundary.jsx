import { Component } from 'react';
import { Box, Typography, Button, Paper, Container } from '@mui/material';
import { Error as ErrorIcon, Refresh as RefreshIcon } from '@mui/icons-material';

/**
 * Error Boundary component that catches React rendering errors
 * and displays a fallback UI instead of crashing the entire app
 */
export class ErrorBoundary extends Component {
    constructor(props) {
        super(props);
        this.state = {
            hasError: false,
            error: null,
            errorInfo: null
        };
    }

    static getDerivedStateFromError() {
        // Update state so the next render will show the fallback UI
        return { hasError: true };
    }

    componentDidCatch(error, errorInfo) {
        // Log error details
        console.error('ErrorBoundary caught an error:', error, errorInfo);

        // You can also log to an error reporting service here
        this.setState({
            error,
            errorInfo
        });
    }

    handleReload = () => {
        // Clear error state and reload
        this.setState({ hasError: false, error: null, errorInfo: null });
        window.location.reload();
    };

    handleReset = () => {
        // Just clear the error state without reloading
        this.setState({ hasError: false, error: null, errorInfo: null });
    };

    render() {
        if (this.state.hasError) {
            return (
                <Container maxWidth="sm" sx={{ mt: 8 }}>
                    <Paper
                        elevation={3}
                        sx={{
                            p: 4,
                            textAlign: 'center',
                            borderRadius: 2
                        }}
                    >
                        <ErrorIcon
                            sx={{
                                fontSize: 64,
                                color: 'error.main',
                                mb: 2
                            }}
                        />
                        <Typography variant="h4" gutterBottom fontWeight={600}>
                            Oops! Something went wrong
                        </Typography>
                        <Typography variant="body1" color="text.secondary" paragraph>
                            We encountered an unexpected error. Don't worry, your data is safe.
                        </Typography>

                        {/* Show error details in development */}
                        {import.meta.env.DEV && this.state.error && (
                            <Box
                                sx={{
                                    mt: 3,
                                    p: 2,
                                    bgcolor: 'grey.100',
                                    borderRadius: 1,
                                    textAlign: 'left',
                                    maxHeight: 200,
                                    overflow: 'auto'
                                }}
                            >
                                <Typography variant="caption" component="pre" sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>
                                    {this.state.error.toString()}
                                    {this.state.errorInfo && this.state.errorInfo.componentStack}
                                </Typography>
                            </Box>
                        )}

                        <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'center' }}>
                            <Button
                                variant="contained"
                                startIcon={<RefreshIcon />}
                                onClick={this.handleReload}
                            >
                                Reload Page
                            </Button>
                            <Button
                                variant="outlined"
                                onClick={this.handleReset}
                            >
                                Try Again
                            </Button>
                        </Box>
                    </Paper>
                </Container>
            );
        }

        return this.props.children;
    }
}
