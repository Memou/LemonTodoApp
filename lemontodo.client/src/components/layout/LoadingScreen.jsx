import { Box, Typography, CircularProgress } from '@mui/material';

export function LoadingScreen() {
    return (
        <Box 
            display="flex" 
            flexDirection="column"
            justifyContent="center" 
            alignItems="center" 
            minHeight="100vh"
            bgcolor="background.default"
            gap={3}
        >
            <Typography 
                variant="h3" 
                sx={{ 
                    color: 'text.primary',
                    fontWeight: 700,
                    letterSpacing: '-0.03em',
                    textTransform: 'lowercase',
                    mb: 2
                }}
            >
                taskflow
            </Typography>
            <CircularProgress size={50} thickness={4} sx={{ color: '#F7C948' }} />
        </Box>
    );
}
