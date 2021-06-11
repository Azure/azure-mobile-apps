module.exports = function(grunt) {
    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        browserify: {
			files: {
                src: 'index.js',
                dest: './generated/bundle.js'
            }
        }
    });

    // Load the plugins
    grunt.loadNpmTasks('grunt-browserify');
        
    // Define build tasks
    grunt.registerTask('build', ['browserify']);
};
