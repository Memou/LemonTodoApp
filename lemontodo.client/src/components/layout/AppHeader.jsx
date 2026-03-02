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
    IconButton,
} from '@mui/material';
import {
    FileDownload as FileDownloadIcon,
    FileUpload as FileUploadIcon,
    MoreVert as MoreVertIcon,
} from '@mui/icons-material';

export function AppHeader({ username, onLogout, onExport, onImport, tasksCount }) {
    const [exportMenuAnchor, setExportMenuAnchor] = useState(null);
    const [mobileMenuAnchor, setMobileMenuAnchor] = useState(null);

    const handleExport = (format) => {
        onExport(format);
        setExportMenuAnchor(null);
        setMobileMenuAnchor(null);
    };

    const handleMobileExport = (format) => {
        handleExport(format);
    };

    const handleMobileImportClick = () => {
        setMobileMenuAnchor(null);
        setTimeout(() => {
            document.getElementById('mobile-import-input')?.click();
        }, 100);
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
                    {/* Mobile Menu Button */}
                    <IconButton
                        onClick={(e) => setMobileMenuAnchor(e.currentTarget)}
                        sx={{ 
                            display: { xs: 'flex', md: 'none' },
                            color: 'text.primary'
                        }}
                    >
                        <MoreVertIcon />
                    </IconButton>
                    <Menu
                        anchorEl={mobileMenuAnchor}
                        open={Boolean(mobileMenuAnchor)}
                        onClose={() => setMobileMenuAnchor(null)}
                        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
                        slotProps={{
                            paper: {
                                elevation: 3
                            }
                        }}
                    >
                        <MenuItem onClick={() => handleMobileExport('json')} disabled={tasksCount === 0}>
                            <FileDownloadIcon sx={{ mr: 1, fontSize: '1.2rem' }} />
                            Export as JSON
                        </MenuItem>
                        <MenuItem onClick={() => handleMobileExport('csv')} disabled={tasksCount === 0}>
                            <FileDownloadIcon sx={{ mr: 1, fontSize: '1.2rem' }} />
                            Export as CSV
                        </MenuItem>
                        <MenuItem onClick={handleMobileImportClick}>
                            <FileUploadIcon sx={{ mr: 1, fontSize: '1.2rem' }} />
                            Import
                        </MenuItem>
                    </Menu>
                    {/* Hidden file input for mobile import */}
                    <input
                        id="mobile-import-input"
                        type="file"
                        hidden
                        accept=".json,.csv"
                        onChange={onImport}
                    />

                    {/* Desktop Export Button */}
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
                    {/* Desktop Export Menu */}
                    <Menu
                        anchorEl={exportMenuAnchor}
                        open={Boolean(exportMenuAnchor)}
                        onClose={() => setExportMenuAnchor(null)}
                        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
                        slotProps={{
                            paper: {
                                elevation: 3
                            }
                        }}
                    >
                        <MenuItem onClick={() => handleExport('json')}>Export as JSON</MenuItem>
                        <MenuItem onClick={() => handleExport('csv')}>Export as CSV</MenuItem>
                    </Menu>
                    {/* Desktop Import Button */}
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
