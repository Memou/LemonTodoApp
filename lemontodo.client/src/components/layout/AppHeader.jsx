import { useState } from 'react';
import {
    AppBar,
    Toolbar,
    Typography,
    Button,
    Stack,
    Box,
    Divider,
    Menu,
    MenuItem,
} from '@mui/material';
import {
    FileDownload as FileDownloadIcon,
    FileUpload as FileUploadIcon,
} from '@mui/icons-material';

export function AppHeader({ username, onLogout, onExport, onImport, tasksCount }) {
    const [exportMenuAnchor, setExportMenuAnchor] = useState(null);

    const handleExport = (format) => {
        onExport(format);
        setExportMenuAnchor(null);
    };

    return (
        <AppBar position="static" elevation={0} sx={{ bgcolor: 'background.default', borderBottom: 'none' }}>
            <Toolbar sx={{ py: 1.5, px: { xs: 2, sm: 4 } }}>
                <Typography 
                    variant="h6" 
                    sx={{ 
                        flexGrow: 0,
                        mr: 6,
                        color: 'text.primary', 
                        fontWeight: 700, 
                        fontSize: '1.75rem',
                        letterSpacing: '-0.03em',
                        fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", system-ui, sans-serif',
                        textTransform: 'lowercase'
                    }}
                >
                    taskflow
                </Typography>
                <Box sx={{ flexGrow: 1 }} />
                <Stack direction="row" spacing={2} alignItems="center">
                    <Button
                        variant="outlined"
                        size="small"
                        startIcon={<FileDownloadIcon />}
                        onClick={(e) => setExportMenuAnchor(e.currentTarget)}
                        disabled={tasksCount === 0}
                        sx={{ 
                            display: { xs: 'none', md: 'flex' },
                            borderColor: 'divider',
                            color: 'text.primary',
                            '&:hover': { borderColor: 'text.primary' },
                            '&.Mui-disabled': {
                                borderColor: 'divider',
                                color: 'text.disabled'
                            }
                        }}
                    >
                        Export
                    </Button>
                    <Menu
                        anchorEl={exportMenuAnchor}
                        open={Boolean(exportMenuAnchor)}
                        onClose={() => setExportMenuAnchor(null)}
                        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
                    >
                        <MenuItem onClick={() => handleExport('json')}>Export as JSON</MenuItem>
                        <MenuItem onClick={() => handleExport('csv')}>Export as CSV</MenuItem>
                    </Menu>
                    <Button
                        variant="outlined"
                        size="small"
                        startIcon={<FileUploadIcon />}
                        component="label"
                        sx={{ 
                            display: { xs: 'none', md: 'flex' },
                            borderColor: 'divider',
                            color: 'text.primary',
                            '&:hover': { borderColor: 'text.primary' }
                        }}
                    >
                        Import
                        <input
                            type="file"
                            hidden
                            accept=".json,.csv"
                            onChange={onImport}
                        />
                    </Button>
                    <Divider orientation="vertical" flexItem sx={{ mx: 1, display: { xs: 'none', md: 'block' } }} />
                    <Typography 
                        variant="body1" 
                        sx={{ 
                            color: 'text.primary',
                            fontWeight: 500,
                            display: { xs: 'none', sm: 'block' },
                            fontSize: '0.95rem'
                        }}
                    >
                        {username}
                    </Typography>
                    <Button 
                        variant="text" 
                        onClick={onLogout} 
                        sx={{ 
                            color: 'text.primary',
                            fontWeight: 500,
                            fontSize: '0.95rem',
                            px: 2,
                            '&:hover': { bgcolor: 'transparent', color: 'text.secondary' }
                        }}
                    >
                        Sign Out
                    </Button>
                </Stack>
            </Toolbar>
        </AppBar>
    );
}
