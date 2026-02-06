describe('Navigation', () => {
  beforeEach(() => {
    cy.intercept('GET', '/api/invoices*', {
      statusCode: 200,
      body: { items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false },
    });
    cy.intercept('GET', '/api/customers*', {
      statusCode: 200,
      body: { items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false },
    });
    cy.intercept('GET', '/api/senders*', {
      statusCode: 200,
      body: { items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false },
    });
  });

  it('redirects root to /invoices', () => {
    cy.visit('/');
    cy.url().should('include', '/invoices');
    cy.contains('Invoices');
  });

  it('navigates between tabs', () => {
    cy.visit('/invoices');

    cy.contains('a', 'Customers').click();
    cy.url().should('include', '/customers');
    cy.contains('h1', 'Customers');

    cy.contains('a', 'Senders').click();
    cy.url().should('include', '/senders');
    cy.contains('h1', 'Senders');

    cy.contains('a', 'Invoices').click();
    cy.url().should('include', '/invoices');
    cy.contains('h1', 'Invoices');
  });

  it('highlights active tab', () => {
    cy.visit('/customers');
    cy.contains('a', 'Customers').should('have.class', 'bg-indigo-50');
    cy.contains('a', 'Invoices').should('not.have.class', 'bg-indigo-50');
  });

  it('shows Invoice Builder branding', () => {
    cy.visit('/');
    cy.contains('Invoice Builder');
  });
});
