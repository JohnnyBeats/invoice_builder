describe('Senders', () => {
  const mockSenders = {
    items: [
      {
        id: '1',
        companyName: 'My Company',
        contactPerson: 'Alice',
        address: '100 Business Blvd',
        email: 'alice@myco.com',
        phone: '+1234567890',
        vatTaxId: 'SEND-VAT-001',
        iban: 'DE89370400440532013000',
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 1,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  };

  beforeEach(() => {
    cy.intercept('GET', '/api/senders*', { statusCode: 200, body: mockSenders }).as(
      'getSenders'
    );
  });

  it('displays sender list', () => {
    cy.visit('/senders');
    cy.wait('@getSenders');

    cy.contains('Senders');
    cy.contains('My Company');
    cy.contains('alice@myco.com');
    cy.contains('+1234567890');
    cy.contains('DE89370400440532013000');
  });

  it('creates a new sender', () => {
    const newSender = {
      id: '2',
      companyName: 'New Sender Co',
      contactPerson: 'Bob',
      address: '200 Sender St',
      email: 'bob@sender.com',
      phone: '+9876543210',
      vatTaxId: null,
      iban: null,
      createdAt: '2026-02-01T00:00:00Z',
      updatedAt: '2026-02-01T00:00:00Z',
    };

    cy.intercept('POST', '/api/senders', { statusCode: 201, body: newSender }).as(
      'createSender'
    );

    cy.visit('/senders');
    cy.wait('@getSenders');

    cy.contains('button', 'Add Sender').click();
    cy.contains('New Sender');

    cy.get('input').eq(0).type('New Sender Co');
    cy.get('input').eq(1).type('Bob');
    cy.get('input').eq(2).type('200 Sender St');
    cy.get('input').eq(3).type('bob@sender.com');
    cy.get('input').eq(4).type('+9876543210');

    cy.contains('button', 'Create').click();
    cy.wait('@createSender');

    cy.get('@createSender')
      .its('request.body')
      .should('deep.include', {
        companyName: 'New Sender Co',
        phone: '+9876543210',
      });
  });

  it('edits an existing sender', () => {
    cy.intercept('PUT', '/api/senders/1', {
      statusCode: 200,
      body: { ...mockSenders.items[0], companyName: 'Updated Co' },
    }).as('updateSender');

    cy.visit('/senders');
    cy.wait('@getSenders');

    cy.get('table tbody tr').first().find('button[title="Edit"]').click();
    cy.contains('Edit Sender');

    cy.get('input').eq(0).should('have.value', 'My Company');
    cy.get('input').eq(0).clear().type('Updated Co');

    cy.contains('button', 'Update').click();
    cy.wait('@updateSender');

    cy.get('@updateSender')
      .its('request.body')
      .should('deep.include', { companyName: 'Updated Co' });
  });

  it('deletes a sender with confirmation', () => {
    cy.intercept('DELETE', '/api/senders/1', { statusCode: 204 }).as('deleteSender');

    cy.visit('/senders');
    cy.wait('@getSenders');

    cy.get('table tbody tr').first().find('button[title="Delete"]').click();
    cy.contains('Delete Sender');
    cy.contains('button', 'Delete').click();
    cy.wait('@deleteSender');
  });

  it('shows empty state when no senders', () => {
    cy.intercept('GET', '/api/senders*', {
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

    cy.visit('/senders');
    cy.wait('@getEmpty');

    cy.contains('No senders yet');
  });
});
