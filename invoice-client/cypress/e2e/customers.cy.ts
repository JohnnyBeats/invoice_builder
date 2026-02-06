describe('Customers', () => {
  const mockCustomers = {
    items: [
      {
        id: '1',
        companyName: 'Acme Corp',
        contactPerson: 'John Doe',
        address: '123 Main St',
        email: 'john@acme.com',
        postalCode: '12345',
        vatTaxId: 'VAT-001',
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
      {
        id: '2',
        companyName: 'Widget Inc',
        contactPerson: 'Jane Smith',
        address: '456 Oak Ave',
        email: 'jane@widget.com',
        postalCode: '67890',
        vatTaxId: null,
        createdAt: '2026-01-02T00:00:00Z',
        updatedAt: '2026-01-02T00:00:00Z',
      },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 2,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  };

  beforeEach(() => {
    cy.intercept('GET', '/api/customers*', { statusCode: 200, body: mockCustomers }).as(
      'getCustomers'
    );
  });

  it('displays customer list with data', () => {
    cy.visit('/customers');
    cy.wait('@getCustomers');

    cy.contains('Customers');
    cy.contains('Acme Corp');
    cy.contains('Widget Inc');
    cy.contains('john@acme.com');
    cy.contains('VAT-001');
    cy.contains('Showing 1 to 2 of 2 results');
  });

  it('opens create customer modal and submits', () => {
    const newCustomer = {
      id: '3',
      companyName: 'New Co',
      contactPerson: 'Bob',
      address: '789 Pine Rd',
      email: 'bob@newco.com',
      postalCode: '11111',
      vatTaxId: null,
      createdAt: '2026-02-01T00:00:00Z',
      updatedAt: '2026-02-01T00:00:00Z',
    };

    cy.intercept('POST', '/api/customers', { statusCode: 201, body: newCustomer }).as(
      'createCustomer'
    );

    cy.visit('/customers');
    cy.wait('@getCustomers');

    cy.contains('button', 'Add Customer').click();
    cy.contains('New Customer');

    cy.get('input').eq(0).type('New Co');
    cy.get('input').eq(1).type('Bob');
    cy.get('input').eq(2).type('789 Pine Rd');
    cy.get('input').eq(3).type('bob@newco.com');
    cy.get('input').eq(4).type('11111');

    cy.contains('button', 'Create').click();
    cy.wait('@createCustomer');

    cy.get('@createCustomer')
      .its('request.body')
      .should('deep.include', {
        companyName: 'New Co',
        contactPerson: 'Bob',
        email: 'bob@newco.com',
      });
  });

  it('opens edit modal with pre-filled data', () => {
    cy.intercept('PUT', '/api/customers/1', {
      statusCode: 200,
      body: { ...mockCustomers.items[0], companyName: 'Acme Updated' },
    }).as('updateCustomer');

    cy.visit('/customers');
    cy.wait('@getCustomers');

    // Click edit on first row
    cy.get('table tbody tr').first().find('button[title="Edit"]').click();
    cy.contains('Edit Customer');

    // Verify pre-filled values
    cy.get('input').eq(0).should('have.value', 'Acme Corp');
    cy.get('input').eq(1).should('have.value', 'John Doe');

    // Update company name
    cy.get('input').eq(0).clear().type('Acme Updated');
    cy.contains('button', 'Update').click();
    cy.wait('@updateCustomer');

    cy.get('@updateCustomer')
      .its('request.body')
      .should('deep.include', { companyName: 'Acme Updated' });
  });

  it('shows delete confirmation and deletes customer', () => {
    cy.intercept('DELETE', '/api/customers/1', { statusCode: 204 }).as('deleteCustomer');

    cy.visit('/customers');
    cy.wait('@getCustomers');

    cy.get('table tbody tr').first().find('button[title="Delete"]').click();

    cy.contains('Delete Customer');
    cy.contains('This action cannot be undone');

    cy.contains('button', 'Delete').click();
    cy.wait('@deleteCustomer');
  });

  it('shows empty state when no customers exist', () => {
    cy.intercept('GET', '/api/customers*', {
      statusCode: 200,
      body: {
        items: [],
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false,
      },
    }).as('getEmpty');

    cy.visit('/customers');
    cy.wait('@getEmpty');

    cy.contains('No customers yet');
    cy.contains('Get started by adding your first customer');
  });

  it('shows error state when API fails', () => {
    cy.intercept('GET', '/api/customers*', { statusCode: 500, body: {} }).as('getFail');

    cy.visit('/customers');
    cy.wait('@getFail');

    cy.contains('Failed to load customers');
    cy.contains('Try again');
  });
});
