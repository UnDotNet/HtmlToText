/* eslint-disable no-template-curly-in-string */

// to run with no package.json or locally installed packages:
// this includes dotnet and changelogen plugins
//
// npx release-it-dotnet
//

module.exports = {
  plugins: {
    'release-it-changelogen': {
      excludeAuthors: ['John Campion'],
    },
    'release-it-dotnet': {
      publish: true,
      csprojFile: './src/UnDotNet.HtmlToText.csproj',
    },
  },
  git: {
    requireCleanWorkingDir: false,
    tagName: 'v${version}',
    commitMessage: 'chore(release): v${version}',
  },
  npm: {
    publish: false,
  },
}
