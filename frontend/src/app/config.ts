export const environment = {
  production: window.location.hostname !== 'localhost',
  apiBaseUrl: window.location.hostname === 'localhost' 
    ? 'http://localhost:5048' 
    : 'https://api-complaint-system-gecpaehth3fff3gq.centralindia-01.azurewebsites.net'
};
