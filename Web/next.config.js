const withMDX = require('@next/mdx')();

module.exports = withMDX({
  trailingSlash: true,
  pageExtensions: ['js', 'mdx', 'tsx'],
});
